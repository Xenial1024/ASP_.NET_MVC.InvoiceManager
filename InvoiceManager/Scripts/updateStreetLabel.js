function updateStreetLabel(streetTypeSelect, streetLabel) {
    if (!streetTypeSelect || !streetLabel) return;

    const selectedType = streetTypeSelect.options[streetTypeSelect.selectedIndex].text;

    switch (selectedType) {
        case "ulica": streetLabel.textContent = "Ulica:"; break;
        case "aleja": streetLabel.textContent = "Aleja:"; break;
        case "plac": streetLabel.textContent = "Plac:"; break;
        case "skwer": streetLabel.textContent = "Skwer:"; break;
        case "bulwar": streetLabel.textContent = "Bulwar:"; break;
        case "rondo": streetLabel.textContent = "Rondo:"; break;
        case "park": streetLabel.textContent = "Park:"; break;
        case "rynek": streetLabel.textContent = "Rynek:"; break;
        case "szosa": streetLabel.textContent = "Szosa:"; break;
        case "droga": streetLabel.textContent = "Droga:"; break;
        case "osiedle": streetLabel.textContent = "Osiedle:"; break;
        case "ogród": streetLabel.textContent = "Ogród:"; break;
        case "wyspa": streetLabel.textContent = "Wyspa:"; break;
        default: streetLabel.textContent = "Wybrzeże:"; break;
    }
}