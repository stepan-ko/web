public class PlateAnalyseFactory : IPlateAnalyseFactory
{
    private readonly IServiceProvider _sp;

    public PlateAnalyseFactory(IServiceProvider sp)
    {
        _sp = sp;
    }

    public PlateAnalyse Create(Camera camera)
    {
        var analyse = _sp.GetRequiredService<PlateAnalyse>();
        analyse.Init(camera);
        return analyse;
    }
}