// Vänta tills hela HTML-dokumentet har laddats innan koden körs
document.addEventListener("DOMContentLoaded", function () {
    const rowsPerPage = 10; // Antal rader att visa per sida
    let currentPage = 1; // Den aktuella sidan (startar på 1)

    // Hämta relevanta DOM-element
    const table = document.getElementById("productsTable");
    const tbody = document.getElementById("productsBody");

    const searchInput = document.getElementById("searchInput"); // Sökfältet
    const pagination = document.getElementById("paginationButtons"); // Behållare för sidnavigeringsknappar

    const allOriginalRows = Array.from(tbody.getElementsByTagName("tr")); // Alla rader i tabellen

    let allFilterableRows = [...allOriginalRows]; // Kopia av rader, som kan filtreras
    let currentSortedColumn = { column: null, ascending: true }; // Håller koll på vilken kolumn som är sorterad och i vilken riktning

    let filteredRowIndexes = Array.from(allFilterableRows).map((_, i) => i); // Alla rader visas initialt
   
    function displayOnlyFilteredRowsWithPagination() {

        // Gömmer alla rader, för att börja om från en "ren" visning.
        //Varje rad sätts till display: none, så att den inte syns.
        allOriginalRows.forEach(row => row.style.display = "none");

        //Räknar ut startindex för vilken rad som ska visas på nuvarande sida(currentPage).
        //Exempel: Om currentPage = 2 och rowsPerPage = 10, börjar vi på rad (2 - 1) * 10 = 10.
        const startIndexForCurrentPageRows = (currentPage - 1) * rowsPerPage;

        //Räknar ut slutindex(icke - exklusivt) – alltså den sista raden som ska visas.
        //Ex: startIndex = 10 → endIndex = 20 → vi visar rad 10 till 19 (index).
        const endIndexForCurrentPageRows = startIndexForCurrentPageRows + rowsPerPage;

        // Visa endast rader för den aktuella sidan
        for (let i = startIndexForCurrentPageRows; i < endIndexForCurrentPageRows && i < filteredRowIndexes.length; i++) {
            const row = allOriginalRows[filteredRowIndexes[i]];
            row.style.display = "";
        }
    }

    // Funktion för att skapa och visa sidnavigeringsknappar
    function renderPaginationButtons() {
        pagination.innerHTML = ""; // Töm tidigare knappar

        const totalPages = Math.ceil(filteredRowIndexes.length / rowsPerPage);
        if (totalPages <= 1) return; // Visa inte knappar om bara en sida

        for (let i = 1; i <= totalPages; i++) {
            const btn = document.createElement("button");
            btn.textContent = i;
            btn.classList.add("pagination-btn");

            if (i === currentPage) {
                
                btn.classList.add("activePageButton");//Css klass
            }

            btn.addEventListener("click", () => {
                currentPage = i;
                displayOnlyFilteredRowsWithPagination();
                renderPaginationButtons(); // Uppdatera knapparnas stil
            });

            pagination.appendChild(btn);
        }
    }

    // Funktion för att filtrera rader baserat på söktext.
    //  Raderna döljs bara vid filtrering vilket möjliggör att alla rader kommer med i input-form även
    //  fast dom är bortfiltrerade för tillfället.
    function filterRowsBasedOnSearchInput() {
        const queryFromSearchInput = searchInput.value.toLowerCase();
        const rowsInTable = document.querySelectorAll("#productsBody tr");

        filteredRowIndexes = []; // Håller reda på vilka rader som ska visas

        rowsInTable.forEach((row, index) => {
            const matchToTextContent = Array.from(row.cells).some(cell =>
                cell.textContent.toLowerCase().includes(queryFromSearchInput)
            );

            if (matchToTextContent) {
                filteredRowIndexes.push(index);
            }

            // Dölj alla rader initialt – visas senare av paginering
            row.style.display = "none";
        });

        currentPage = 1;
        displayOnlyFilteredRowsWithPagination();
        renderPaginationButtons();
    }

    function updateTableHeadSortArrows() {
        const headersInTable = table.querySelectorAll("th[data-column]");

        headersInTable.forEach((th, index) => {
            const label = th.getAttribute("data-label");

            if (!label) return; // Hoppa över kolumner utan sortering

            th.innerHTML = label; // Återställ rubriken utan pil

            // Lägg till pil på aktuell kolumn
            if (index === currentSortedColumn.column) {
                th.innerHTML += currentSortedColumn.ascending ? " &#9660;" : " &#9650;";//Pil ner och pil upp
            }
        });
    }

    // Funktion för att sortera rader enligt angiven kolumn
    function sortByColumn(index) {

        //Kollar om den kolumn som just nu klickats är samma som den som redan är sorterad (currentSortedColumn.column).
        //Om det är samma kolumn, så växlar sorteringsordningen (t.ex. från stigande till fallande).
        //Om det är en ny kolumn, börjar sorteringen i stigande ordning.
        //Resultatet sparas i ascending (en boolean).
        const ascending = currentSortedColumn.column === index ? !currentSortedColumn.ascending : true;

        // Sortera filteredRowIndexes baserat på vald kolumn
        //Sorterar listan filteredRowIndexes, som innehåller index till de rader som ska visas (d.v.s. efter ev. filtrering).
        //a och b är index till rader som ska jämföras i sorteringen.
        filteredRowIndexes.sort((a, b) => {

            //Hämtar textinnehållet i cellen i kolumn index för respektive rad.
            //.trim() tar bort mellanslag i början och slutet.
            //.toLowerCase() gör att jämförelsen blir skiftlägesokänslig (t.ex. "Apple" och "apple" behandlas lika).
            //const aText = allOriginalRows[a].cells[index].textContent.trim().toLowerCase();
            //const bText = allOriginalRows[b].cells[index].textContent.trim().toLowerCase();
            const aTableCellText = allOriginalRows[a].cells[index].textContent.trim().toLowerCase();
            const bTableCellText = allOriginalRows[b].cells[index].textContent.trim().toLowerCase();

            //Försöker omvandla texten till ett numeriskt värde.
            //Ersätter , med . för att hantera t.ex. svenska decimaltal (t.ex. "3,14" → "3.14").
            //parseFloat konverterar till flyttal (decimaltal).
            const aTryTextAsANumber = parseFloat(aTableCellText.replace(',', '.'));
            const bTryTextAsANumber = parseFloat(bTableCellText.replace(',', '.'));

            //Om båda värdena kunde tolkas som tal.
            const aIsNumber = /^\d+([.,]\d+)?$/.test(aTableCellText);
            const bIsNumber = /^\d+([.,]\d+)?$/.test(bTableCellText);

            if (aIsNumber && bIsNumber) {

                //Sortera numeriskt.
                //Om ascending är true, sorteras i stigande ordning(aNum - bNum).
                //Annars i fallande ordning.
                return ascending ? aTryTextAsANumber - bTryTextAsANumber : bTryTextAsANumber - aTryTextAsANumber;
            }

            //Om minst ett av värdena inte är ett tal:
            //Sortera alfabetiskt med localeCompare() (tar hänsyn till språkliga regler).
            //Sorteringsriktning beror på ascending.
            return ascending ? aTableCellText.localeCompare(bTableCellText, 'sv') : bTableCellText.localeCompare(aTableCellText, 'sv');
        });

        //Sparar information om vilken kolumn som nu är sorterad, och i vilken riktning (stigande/fallande).
        //Används för att veta hur nästa klick ska hanteras.
        currentSortedColumn = { column: index, ascending };

        // Flytta om rader i DOM enligt sorteringsresultat
        const tbody = document.querySelector("tbody");
        filteredRowIndexes.forEach(index => {
            tbody.appendChild(allOriginalRows[index]);
        });

        displayOnlyFilteredRowsWithPagination();
        renderPaginationButtons();
        updateTableHeadSortArrows();
    }

    // Händelse för sökfältet så att det filtrerar medan man skriver
    searchInput.addEventListener("input", filterRowsBasedOnSearchInput);

    // Klickhändelser på tabellhuvuden för att möjliggöra sortering
    const tableColumnHeaders = table.querySelectorAll("thead th");
    tableColumnHeaders.forEach((th, index) => {
        th.style.cursor = "pointer"; // Ändra muspekaren till pekande hand
        th.addEventListener("click", () => sortByColumn(index)); // Sortera efter vald kolumn
    });

    // Klickhändelser på varje cell för att navigera till produktdetaljer
    document.querySelectorAll("#productsTable td[data-product-id]").forEach(cell => {
        cell.addEventListener("click", (event) => {

            // event.stopPropagation() – Förhindrar att klick händelsen bubblar upp och
            // påverkar andra element(som form eller andra klickbara saker).
            event.stopPropagation();

            const productId = cell.getAttribute("data-product-id");
            if (productId) {
                window.location.href = `SpecificProductDetails?id=${productId}`;
            }
        });
    });

    displayOnlyFilteredRowsWithPagination();
    renderPaginationButtons();
});

