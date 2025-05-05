public class ContactViewModel
{
    public string Title => "Kontakt";
    public string Addresses => @"
        <strong>Email:</strong> <a href=""mailto:admin@email.pl"">admin@email.pl</a><br>
        <strong>Nr tel.:</strong> <a href=""tel:+48000123456"">000 123 456</a><br>
        <strong>Github:</strong> <a href=""https://github.com/Xenial1024"">Xenial1024</a>";

    public string OfficeInfo => @"
        <strong style=""white-space: nowrap;"">🕒 Godziny otwarcia:</strong><br>
        - <b>Pon - Pt:</b> 8:00 - 18:00<br>
        - <b>Sobota:</b> 9:00 - 14:00<br>
        - <b>Niedziela:</b> Zamknięte <br><br>
        <strong style=""white-space: nowrap;"">♿ Udogodnienia dla niepełnosprawnych:</strong><br>
        - 🧏‍ kontakt w języku migowym<br>
        - ↗️  podjazd<br>
        -  🛗  winda";

    public string RouteInfo => @"
        <strong>🗺️ Adres biura:</strong> <a href=""https://www.google.com/maps/search/?api=1&query=ul.+Asynchroniczna+64,+Programowo"">
        12-345 Programowo, ul. Asynchroniczna 64</a><br><br>

        <strong style=""white-space: nowrap;"">🚆 Dojazd komunikacją miejską:</strong><br>
        🚋 <b>Tramwaj:</b> Linie 2, 4, 8 (przystanek „Centrum”)<br>
        🚌 <b>Autobus:</b> Linie 16, 32, 64 (przystanek „Rynek”)";
}