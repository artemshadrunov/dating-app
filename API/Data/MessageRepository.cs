using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddMessage(Message message) => _context.Messgaes.Add(message);

        public void DeleteMessage(Message message) => _context.Messgaes.Remove(message);

        public async Task<Message> GetMessage(int id) => await _context.Messgaes
                                                                    .Include(u => u.Sender)
                                                                    .Include(u => u.Recipient)
                                                                    .SingleOrDefaultAsync(x => x.Id == id);

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messgaes
                .OrderByDescending(m => m.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(m => !m.RecipientDeleted && m.Recipient.UserName == messageParams.Username),
                "Outbox" => query.Where(m => !m.SenderDeleted && m.Sender.UserName == messageParams.Username),
                _ => query.Where(m => m.DateRead == null && !m.RecipientDeleted && m.Recipient.UserName == messageParams.Username)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await _context.Messgaes
                .Include(m => m.Sender).ThenInclude(p => p.Photos)
                .Include(m => m.Recipient).ThenInclude(p => p.Photos)
                .Where(m => !m.RecipientDeleted && 
                        m.Recipient.UserName == currentUsername && 
                        m.Sender.UserName == recipientUsername || 
                        !m.SenderDeleted &&
                        m.Recipient.UserName == recipientUsername &&
                        m.Sender.UserName == currentUsername
                )
                .OrderBy(m => m.MessageSent)
                .ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null && m.Recipient.UserName == currentUsername).ToList();
            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}