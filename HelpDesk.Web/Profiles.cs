using System;
using AutoMapper;
using HelpDesk.Models.ViewModels;
using HelpDesk.Models.Entities;
using HelpDesk.Models.IdentityEntities;

namespace HelpDesk.Web
{
    public class Profiles
    {
        //AutoMapper kullanabilmek amacıyla, maplenecek her model için Profile'dan kalıtım almış classlar kullanılmalı.
        public class FailureProfile : Profile
        {
            public FailureProfile()
            {
                CreateMap<Failure, FailureViewModel>()
                    .ForMember(dest => dest.FailureId, opt => opt.MapFrom(x => x.Id))
                    .ForMember(dest => dest.CreatedTime,
                        opt => opt.MapFrom((s, d) => s.CreatedDate == null ? DateTime.Now : s.CreatedDate))
                    .ForMember(dest => dest.Operator,
                        opt => opt.MapFrom(
                            (s, d) => s.Operator == null ? "-" : (s.Operator.Name + " " + s.Operator.Surname)))
                    .ForMember(dest => dest.Technician,
                        opt => opt.MapFrom((s, d) =>
                            s.Technician == null ? "-" : (s.Technician.Name + " " + s.Technician.Surname)))
                    .ForMember(dest => dest.TechnicianStatus, opt => opt.MapFrom(x => x.Technician.TechnicianStatus))
                    .ForMember(dest => dest.PhotoPath, opt => opt.MapFrom(x => x.PhotoPath))
                    .ForMember(dest => dest.ClientName, opt => opt.MapFrom(x => x.Client.Name))
                    .ForMember(dest => dest.ClientSurname, opt => opt.MapFrom(x => x.Client.Surname));

                CreateMap<FailureViewModel, Failure>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.FailureId));
            }
        }

        public class FailureLogProfile : Profile
        {
            public FailureLogProfile()
            {
                CreateMap<FailureLog, FailureLogViewModel>().ReverseMap();
            }
        }

        public class UserProfileProfile : Profile
        {
            public UserProfileProfile()
            {
                CreateMap<ApplicationUser, UserProfileViewModel>().ReverseMap();
            }
        }

        //Çoğaltılabilir...
        //Controller'da kullanmak için IMapper ile DI yapılmalı.
    }
}
