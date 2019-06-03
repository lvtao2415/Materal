﻿using AutoMapper;
using Materal.Common;
using Materal.ConvertHelper;
using Materal.LinqHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Authority.DataTransmitModel.User;
using Authority.Domain;
using Authority.Domain.Repositories;
using Authority.EFRepository;
using Authority.Service;
using Authority.Service.Model.User;
namespace Authority.ServiceImpl
{
    /// <summary>
    /// 用户服务
    /// </summary>
    public sealed class UserServiceImpl : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IAuthorityUnitOfWork _authorityUnitOfWork;
        public UserServiceImpl(IUserRepository userRepository, IMapper mapper, IAuthorityUnitOfWork authorityUnitOfWork)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _authorityUnitOfWork = authorityUnitOfWork;
        }

        public async Task AddUserAsync(AddUserModel model)
        {
            throw new NotImplementedException();
        }

        public async Task EditUserAsync(EditUserModel model)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteUserAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDTO> GetUserInfoAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDTO> GetUserInfoAsync(string token)
        {
            throw new NotImplementedException();
        }

        public Guid GetUserID(string token)
        {
            throw new NotImplementedException();
        }

        public async Task<(List<UserListDTO> result, PageModel pageModel)> GetUserListAsync(QueryUserFilterModel filterModel)
        {
            throw new NotImplementedException();
        }

        public async Task ExchangePasswordAsync(ChangePasswordModel model)
        {
            throw new NotImplementedException();
        }

        public async Task<string> ResetPasswordAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<UserListDTO> LoginAsync(string account, string password)
        {
            throw new NotImplementedException();
        }

        public string GetEncodePassword(string password)
        {
            throw new NotImplementedException();
        }
    }
}