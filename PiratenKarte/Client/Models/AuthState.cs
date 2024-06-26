﻿using PiratenKarte.Shared;

namespace PiratenKarte.Client.Models;

public class AuthState {
    public UserDTO? User { get; set; }
    public string? Token { get; set; }
    public DateTime TokenValidTill { get; set; }
    public List<PermissionDTO> Permissions { get; set; } = new();
    public bool KeepLoggedIn { get; set; }
}