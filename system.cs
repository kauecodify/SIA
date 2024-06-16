using System;
using System.Threading.Tasks;
using GoogleMaps.LocationServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

//executar no terminal >>
//dotnet new console -n "system"
//cd system

//--------------//

//instalar aplicações (terminal)
//dotnet add package Microsoft.EntityFrameworkCore.Design
//dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
//dotnet add package GoogleMaps.LocationServices

//--------------//
class Program
{
    private static GoogleMaps.Map mapa; // armazenar o mapa globalmente

    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

        using (var dbContext = new ApplicationDbContext(optionsBuilder.Options))
        {
            await AdicionarEvento(dbContext);
            await ExibirEventosNoMapa(dbContext);
        }
    }

    static async Task AdicionarEvento(ApplicationDbContext dbContext)
    {
        var endereco = "Rua Exemplo, 123, Cidade, Estado";
        var nomeEvento = "Evento de Exemplo";
        var segmentos = "sem glúten, comida fresca";
        var descricaoEvento = "Descrição do evento de exemplo";
        var descricaoLocal = "Local aconchegante com opções saudáveis";

        var locationService = new GoogleLocationService();
        var point = await locationService.GetLatLongFromAddressAsync(endereco);

        if (point != null)
        {
            var evento = new Evento
            {
                Endereco = endereco,
                NomeEvento = nomeEvento,
                DescricaoEvento = descricaoEvento,
                Segmentos = segmentos,
                Latitude = point.Latitude,
                Longitude = point.Longitude,
                DescricaoLocal = descricaoLocal
            };

            dbContext.Eventos.Add(evento);
            await dbContext.SaveChangesAsync();

            Console.WriteLine($"Evento '{nomeEvento}' cadastrado com sucesso no banco de dados.");

            // configura o mapa apenas uma vez no início
            if (mapa == null)
            {
                mapa = new GoogleMaps.Map("map", new GoogleMaps.MapOptions
                {
                    Center = new GoogleMaps.LatLng(0, 0),
                    Zoom = 2
                });
            }

            // exibe o evento recém-cadastrado no mapa
            await ExibirEventoNoMapa(evento, mapa);
        }
        else
        {
            Console.WriteLine("Endereço não encontrado.");
        }
    }

    static async Task ExibirEventosNoMapa(ApplicationDbContext dbContext)
    {
        // configura o mapa apenas uma vez no início
        if (mapa == null)
        {
            mapa = new GoogleMaps.Map("map", new GoogleMaps.MapOptions
            {
                Center = new GoogleMaps.LatLng(0, 0),
                Zoom = 2
            });
        }

        // eventos cadastrados do banco de dados
        var eventos = await dbContext.Eventos.ToListAsync();

        // adc marcadores para cada evento no mapa
        foreach (var evento in eventos)
        {
            await ExibirEventoNoMapa(evento, mapa);
        }
    }

    static async Task ExibirEventoNoMapa(Evento evento, GoogleMaps.Map mapa)
    {
        var location = new GoogleMaps.LatLng(evento.Latitude, evento.Longitude);

        var marker = new GoogleMaps.Marker(new GoogleMaps.MarkerOptions
        {
            Position = location,
            Map = mapa,
            Title = evento.NomeEvento
        });

        // infowindow para exibir detalhes do evento
        var infowindow = new GoogleMaps.InfoWindow(new GoogleMaps.InfoWindowOptions
        {
            Content = $"<h3>{evento.NomeEvento}</h3><p>{evento.DescricaoEvento}</p>"
        });

        // abre infowindow ao clicar no marcador
        marker.AddListener("click", () =>
        {
            infowindow.Open(mapa, marker);
        });
    }
}
