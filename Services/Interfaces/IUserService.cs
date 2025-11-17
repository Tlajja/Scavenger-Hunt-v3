using System.Collections.Generic;
using PhotoScavengerHunt.Features.Users;

namespace PhotoScavengerHunt.Services.Interfaces;

public interface IUserService
{
    Task<(bool Success, string Error, UserProfile? User)> CreateUserAsync(string name, int age);
    Task<(bool Success, string Error, List<UserProfile>? Users)> GetUsersAsync();
    Task<(bool Success, string Error, UserProfile? User)> GetUserByIdAsync(int id);
    Task<(bool Success, string Error)> DeleteUserAsync(int id);
}

