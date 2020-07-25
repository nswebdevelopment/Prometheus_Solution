using AutoMapper;
using Prometheus.Common.Enums;
using Prometheus.Dal;
using System;

namespace Prometheus.Model
{
    public class MappingEntity : Profile
    {
        public MappingEntity()
        {
            CreateMap<Cryptoadapter, Adapter>()
                //.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Direction, opt => opt.Ignore())
                .ForMember(dest => dest.DirectionId, opt => opt.ResolveUsing(src => MapDirection(src.Direction)))
                //.ForMember(dest => dest.UserProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.StatusId, opt => opt.UseValue((int)Statuses.Active))
                .ForMember(dest => dest.AdapterTypeItemId, opt => opt.ResolveUsing(src=> (int)MapAdapterTypeItem(src.Type)));

            CreateMap<Cryptoadapter, CryptoAdapter>()
                //.ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RpcAddr, opt => opt.MapFrom(src => src.RpcAddress))
                .ForMember(dest => dest.RpcPort, opt => opt.MapFrom(src => src.RpcPort));
                //.ForMember(dest => dest.AdapterId, opt => opt.Ignore());

            CreateMap<Property, CryptoAdapterProperty>()
                //.ForMember(dest => dest.CryptoAdapterId, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyId, opt => opt.ResolveUsing(src=> (int)MapPropertyName(src.Name)))
                .ForMember(dest => dest.Value, opt => opt.ResolveUsing(src => src.Value));

            CreateMap<Businessadapter, Adapter>()
                //.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Direction, opt => opt.Ignore())
                .ForMember(dest => dest.DirectionId, opt => opt.ResolveUsing(src => MapDirection(src.Direction)))
                //.ForMember(dest => dest.UserProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.StatusId, opt => opt.UseValue((int)Statuses.Active))
                .ForMember(dest => dest.AdapterTypeItemId, opt => opt.ResolveUsing(src => (int)MapAdapterTypeItem(src.Type)));

            CreateMap<Businessadapter, BusinessAdapter>()
                //.ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Filename, opt => opt.MapFrom(src => src.Filename));
                //.ForMember(dest => dest.AdapterId, opt => opt.Ignore());
        }

        public static DirectionEnum MapDirection(string direction)
        {
            Enum.TryParse(direction, out DirectionEnum result);

            return result;
        }

        public static PropertyEnum MapPropertyName(string propertyName)
        {
            Enum.TryParse(propertyName, out PropertyEnum result);

            return result;
        }

        public static AdapterTypeItemEnum MapAdapterTypeItem(string adapterTypeItemName)
        {
            Enum.TryParse(adapterTypeItemName, out AdapterTypeItemEnum result);

            return result;
        }
    }
}
