namespace GestaoResiduosESG.Domain.Entities;

public class LimiteArmazenamento
{
    public int Id { get; set; }
    public int PontoColetaId { get; set; }
    public int ResiduoId { get; set; }
    public decimal LimiteMinimo { get; set; }
    public decimal LimiteMaximo { get; set; }
    public int PercentualAlerta { get; set; } = 80;

    public PontoColeta PontoColeta { get; set; } = null!;
    public Residuo Residuo { get; set; } = null!;
}
