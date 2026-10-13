using AutoMapper;
using GestaoResiduosESG.Application.ViewModels.Alerta;
using GestaoResiduosESG.Application.ViewModels.Descarte;
using GestaoResiduosESG.Application.ViewModels.Limite;
using GestaoResiduosESG.Application.ViewModels.PontoColeta;
using GestaoResiduosESG.Application.ViewModels.Residuo;
using GestaoResiduosESG.Domain.Entities;

namespace GestaoResiduosESG.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Residuo
        CreateMap<Residuo, ResiduoResponseViewModel>()
            .ForMember(d => d.Categoria, opt => opt.MapFrom(src => src.Categoria.ToString()));
        CreateMap<ResiduoCreateViewModel, Residuo>();
        CreateMap<ResiduoUpdateViewModel, Residuo>();

        // PontoColeta
        CreateMap<PontoColeta, PontoColetaResponseViewModel>();
        CreateMap<PontoColetaCreateViewModel, PontoColeta>()
            .ForMember(d => d.QuantidadeAtual, opt => opt.Ignore());
        CreateMap<PontoColetaUpdateViewModel, PontoColeta>()
            .ForMember(d => d.QuantidadeAtual, opt => opt.Ignore());

        // Descarte — enriquece resposta com nomes para evitar IDs soltos
        CreateMap<Descarte, DescarteResponseViewModel>()
            .ForMember(d => d.NomeResiduo, opt => opt.MapFrom(src => src.Residuo.Nome))
            .ForMember(d => d.NomePontoColeta, opt => opt.MapFrom(src => src.PontoColeta.Nome));
        CreateMap<DescarteCreateViewModel, Descarte>();

        // LimiteArmazenamento
        CreateMap<LimiteArmazenamento, LimiteResponseViewModel>()
            .ForMember(d => d.NomePontoColeta, opt => opt.MapFrom(src => src.PontoColeta.Nome))
            .ForMember(d => d.NomeResiduo, opt => opt.MapFrom(src => src.Residuo.Nome));
        CreateMap<LimiteCreateViewModel, LimiteArmazenamento>();
        CreateMap<LimiteUpdateViewModel, LimiteArmazenamento>()
            .ForMember(d => d.PontoColetaId, opt => opt.Ignore())
            .ForMember(d => d.ResiduoId, opt => opt.Ignore());

        // Alerta — expõe enums como strings legíveis
        CreateMap<Alerta, AlertaResponseViewModel>()
            .ForMember(d => d.NomePontoColeta, opt => opt.MapFrom(src => src.PontoColeta.Nome))
            .ForMember(d => d.NomeResiduo, opt => opt.MapFrom(src => src.Residuo.Nome))
            .ForMember(d => d.Tipo, opt => opt.MapFrom(src => src.Tipo.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
