﻿namespace PiratenKarte.Shared.RequestModels;

public class LoginData {
    public required string Username { get; init; }
    public required string Password { get; init; }
}