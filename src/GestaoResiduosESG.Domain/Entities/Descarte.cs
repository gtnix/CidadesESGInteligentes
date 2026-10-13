namespace GestaoResiduosESG.Domain.Entities;

public class Descarte
{
    public int Id { get; set; }
    public int ResiduoId { get; set; }
    public int PontoColetaId { get; set; }
    public DateTime DataDescarte { get; set; }
    public decimal Quantidade { get; set; }
    public string Responsavel { get; set; } = string.Empty;

    public Residuo Residuo { get; set; } = null!;
    public PontoColeta PontoColeta { get; set; } = null!;
}
