namespace GestaoResiduosESG.Domain.Entities;

public class PontoColeta
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public decimal CapacidadeMaxima { get; set; }
    public decimal QuantidadeAtual { get; set; }

    public ICollection<Descarte> Descartes { get; set; } = new List<Descarte>();
    public ICollection<PontoResiduo> PontosResiduo { get; set; } = new List<PontoResiduo>();
    public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
    public ICollection<LimiteArmazenamento> LimitesArmazenamento { get; set; } = new List<LimiteArmazenamento>();
}
