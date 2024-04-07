using AutoMapper;
using PiratenKarte.DAL.Models;
using PiratenKarte.Shared;

namespace PiratenKarte.Server;

public class MapperProfile : Profile {
	public MapperProfile() {
		CreateMap<LatitudeLongitude, LatitudeLongitudeDTO>().ReverseMap();
		CreateMap<MapObject, MapObjectDTO>().ReverseMap();
		CreateMap<ObjectComment, ObjectCommentDTO>().ReverseMap();
		CreateMap<StorageDefinition, StorageDefinitionDTO>().ReverseMap();
		CreateMap<User, UserDTO>().ReverseMap();
		CreateMap<Permission, PermissionDTO>().ReverseMap();
		CreateMap<Group, GroupDTO>().ReverseMap();
	}
}