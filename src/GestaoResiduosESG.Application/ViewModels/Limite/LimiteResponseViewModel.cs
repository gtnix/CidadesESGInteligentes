namespace GestaoResiduosESG.Application.ViewModels.Limite;

public class LimiteResponseViewModel
{
    public int Id { get; set; }
    public int PontoColetaId { get; set; }
    public string NomePontoColeta { get; set; } = string.Empty;
    public int ResiduoId { get; set; }
    public string NomeResiduo { get; set; } = string.Empty;
    public decimal LimiteMinimo { get; set; }
    public decimal LimiteMaximo { get; set; }
    public int PercentualAlerta { get; set; }
}
