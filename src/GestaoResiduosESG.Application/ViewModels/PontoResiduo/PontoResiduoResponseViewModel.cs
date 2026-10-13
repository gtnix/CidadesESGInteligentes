namespace GestaoResiduosESG.Application.ViewModels.PontoResiduo;

public class PontoResiduoResponseViewModel
{
    public int PontoColetaId { get; set; }
    public int ResiduoId { get; set; }
    public string NomeResiduo { get; set; } = string.Empty;
    public string CategoriaResiduo { get; set; } = string.Empty;
    public string UnidadeMedida { get; set; } = string.Empty;
}
