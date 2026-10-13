namespace GestaoResiduosESG.Domain.Entities;

public class PontoResiduo
{
    public int PontoColetaId { get; set; }
    public int ResiduoId { get; set; }

    public PontoColeta PontoColeta { get; set; } = null!;
    public Residuo Residuo { get; set; } = null!;
}
