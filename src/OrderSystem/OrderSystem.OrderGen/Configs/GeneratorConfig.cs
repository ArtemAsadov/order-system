namespace OrderSystem.OrderGen.Configs;

public class GeneratorConfig
{
    public int DefaultRps { get; set; }           // заказов в секунду
    public int MaxUsers { get; set; }      // сколько всего пользователей
    public string[] Categories { get; set; }  // категории товаров
}