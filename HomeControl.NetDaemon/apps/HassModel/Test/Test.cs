using HomeAssistantGenerated;
using NetDaemon.HassModel.Entities;

// Use unique namespaces for your apps if you going to share with others to avoid
// conflicting names
namespace HassModel;

/// <summary>
///     Showcase using the new HassModel API and turn on light on movement
/// </summary>
[NetDaemonApp]
public class Test
{
    private readonly ILogger<Test> _logger;

    public Test(IHaContext ha, ILogger<Test> logger)
    {
        _logger = logger;

        var entities = new Entities(ha);

        entities.Switch.KitchenCounter
            .StateChanges()
            .Where(e => e.New.IsOn())
            .Subscribe(_ =>
            {
                _logger.LogInformation("Lights on");
                entities.Switch.SonoffBasicR2OldRelay.TurnOn();
            });


        entities.Switch.KitchenCounter
            .StateChanges()
            .Where(e => e.New.IsOff())
            .Subscribe(_ => entities.Switch.SonoffBasicR2OldRelay.TurnOff());
       

    }
}
