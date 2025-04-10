﻿using ActiLink.Model;

namespace ActiLink.Repositories
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly ApiContext _context;
        private IRepository<User>? _userRepository;
        private IRepository<Event>? _eventRepository;
        private IRepository<RefreshToken>? _refreshTokenRepository;

        public UnitOfWork(ApiContext context)
        {
            _context = context;
        }

        public IRepository<User> UserRepository => _userRepository ??= new Repository<User>(_context);
        public IRepository<Event> EventRepository => _eventRepository ??= new Repository<Event>(_context);
        public IRepository<RefreshToken> RefreshTokenRepository => _refreshTokenRepository ??= new Repository<RefreshToken>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
