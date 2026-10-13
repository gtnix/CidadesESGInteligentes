namespace GestaoResiduosESG.Application.ViewModels.Alerta;

public class AlertaResponseViewModel
{
    public int Id { get; set; }
    public int PontoColetaId { get; set; }
    public string NomePontoColeta { get; set; } = string.Empty;
    public int ResiduoId { get; set; }
    public string NomeResiduo { get; set; } = string.Empty;
    public DateTime DataAlerta { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
