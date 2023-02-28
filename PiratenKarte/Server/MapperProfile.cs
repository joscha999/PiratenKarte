using AutoMapper;

namespace PiratenKarte.Server;

public class MapperProfile : Profile {
	public MapperProfile() {
		CreateMap<DAL.Models.LatitudeLongitude, Shared.LatitudeLongitude>().ReverseMap();
		CreateMap<DAL.Models.MapObject, Shared.MapObject>().ReverseMap();
		CreateMap<DAL.Models.ObjectComment, Shared.ObjectComment>().ReverseMap();
		CreateMap<DAL.Models.StorageDefinition, Shared.StorageDefinition>().ReverseMap();
		CreateMap<DAL.Models.User, Shared.User>().ReverseMap();
		CreateMap<DAL.Models.Permission, Shared.Permission>().ReverseMap();
	}
}