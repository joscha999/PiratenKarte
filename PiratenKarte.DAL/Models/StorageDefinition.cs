﻿namespace PiratenKarte.DAL.Models;

public class StorageDefinition {
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public LatitudeLongitude Position { get; set; }
}