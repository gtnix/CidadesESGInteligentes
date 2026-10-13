using GestaoResiduosESG.Domain.Enums;

namespace GestaoResiduosESG.Domain.Entities;

public class Residuo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public CategoriaResiduo Categoria { get; set; }
    public string UnidadeMedida { get; set; } = string.Empty;

    public ICollection<Descarte> Descartes { get; set; } = new List<Descarte>();
    public ICollection<PontoResiduo> PontosResiduo { get; set; } = new List<PontoResiduo>();
    public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
    public ICollection<LimiteArmazenamento> LimitesArmazenamento { get; set; } = new List<LimiteArmazenamento>();
}
