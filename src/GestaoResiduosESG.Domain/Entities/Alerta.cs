using GestaoResiduosESG.Domain.Enums;

namespace GestaoResiduosESG.Domain.Entities;

public class Alerta
{
    public int Id { get; set; }
    public int PontoColetaId { get; set; }
    public int ResiduoId { get; set; }
    public DateTime DataAlerta { get; set; }
    public TipoAlerta Tipo { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public StatusAlerta Status { get; set; } = StatusAlerta.Pendente;

    public PontoColeta PontoColeta { get; set; } = null!;
    public Residuo Residuo { get; set; } = null!;
}
