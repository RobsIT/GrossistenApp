// V�nta tills hela HTML-dokumentet har laddats innan koden k�rs
document.addEventListener("DOMContentLoaded", function () {
    const rowsPerPage = 20; // Antal rader att visa per sida
    let currentPage = 1; // Den aktuella sidan (startar p� 1)

    // H�mta relevanta DOM-element
    const table = document.getElementById("productsTable");
    const tbody = document.getElementById("productsBody");

    const searchInput = document.getElementById("searchInput"); // S�kf�ltet
    const pagination = document.getElementById("paginationButtons"); // Beh�llare f�r sidnavigeringsknappar

    const allOriginalRows = Array.from(tbody.getElementsByTagName("tr")); // Alla rader i tabellen

    let allFilterableRows = [...allOriginalRows]; // Kopia av rader, som kan filtreras
    let currentSortedColumn = { column: null, ascending: true }; // H�ller koll p� vilken kolumn som �r sorterad och i vilken riktning

    let filteredRowIndexes = Array.from(allFilterableRows).map((_, i) => i); // Alla rader visas initialt

    function displayOnlyFilteredRowsWithPagination() {

        // G�mmer alla rader, f�r att b�rja om fr�n en "ren" visning.
        //Varje rad s�tts till display: none, s� att den inte syns.
        allOriginalRows.forEach(row => row.style.display = "none");

        //R�knar ut startindex f�r vilken rad som ska visas p� nuvarande sida(currentPage).
        //Exempel: Om currentPage = 2 och rowsPerPage = 10, b�rjar vi p� rad (2 - 1) * 10 = 10.
        const startIndexForCurrentPageRows = (currentPage - 1) * rowsPerPage;

        //R�knar ut slutindex(icke - exklusivt) � allts� den sista raden som ska visas.
        //Ex: startIndex = 10 ? endIndex = 20 ? vi visar rad 10 till 19 (index).
        const endIndexForCurrentPageRows = startIndexForCurrentPageRows + rowsPerPage;

        // Visa endast rader f�r den aktuella sidan
        for (let i = startIndexForCurrentPageRows; i < endIndexForCurrentPageRows && i < filteredRowIndexes.length; i++) {
            const row = allOriginalRows[filteredRowIndexes[i]];
            row.style.display = "";
        }
    }

    // Funktion f�r att skapa och visa sidnavigeringsknappar
    function renderPaginationButtons() {
        pagination.innerHTML = ""; // T�m tidigare knappar

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

    // Funktion f�r att filtrera rader baserat p� s�ktext.
    //  Raderna d�ljs bara vid filtrering vilket m�jligg�r att alla rader kommer med i input-form �ven
    //  fast dom �r bortfiltrerade f�r tillf�llet.
    function filterRowsBasedOnSearchInput() {
        const queryFromSearchInput = searchInput.value.toLowerCase();
        const rowsInTable = document.querySelectorAll("#productsBody tr");

        filteredRowIndexes = []; // H�ller reda p� vilka rader som ska visas

        rowsInTable.forEach((row, index) => {
            const matchToTextContent = Array.from(row.cells).some(cell =>
                cell.textContent.toLowerCase().includes(queryFromSearchInput)
            );

            if (matchToTextContent) {
                filteredRowIndexes.push(index);
            }

            // D�lj alla rader initialt � visas senare av paginering
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

            if (!label) return; // Hoppa �ver kolumner utan sortering

            th.innerHTML = label; // �terst�ll rubriken utan pil

            // L�gg till pil p� aktuell kolumn
            if (index === currentSortedColumn.column) {
                th.innerHTML += currentSortedColumn.ascending ? " &#9660;" : " &#9650;";//Pil ner och pil upp
            }
        });
    }

    // Funktion f�r att sortera rader enligt angiven kolumn
    function sortByColumn(index) {

        //Kollar om den kolumn som just nu klickats �r samma som den som redan �r sorterad (currentSortedColumn.column).
        //Om det �r samma kolumn, s� v�xlar sorteringsordningen (t.ex. fr�n stigande till fallande).
        //Om det �r en ny kolumn, b�rjar sorteringen i stigande ordning.
        //Resultatet sparas i ascending (en boolean).
        const ascending = currentSortedColumn.column === index ? !currentSortedColumn.ascending : true;

        // Sortera filteredRowIndexes baserat p� vald kolumn
        //Sorterar listan filteredRowIndexes, som inneh�ller index till de rader som ska visas (d.v.s. efter ev. filtrering).
        //a och b �r index till rader som ska j�mf�ras i sorteringen.
        filteredRowIndexes.sort((a, b) => {

            //H�mtar textinneh�llet i cellen i kolumn index f�r respektive rad.
            //.trim() tar bort mellanslag i b�rjan och slutet.
            //.toLowerCase() g�r att j�mf�relsen blir skiftl�gesok�nslig (t.ex. "Apple" och "apple" behandlas lika).
            //const aText = allOriginalRows[a].cells[index].textContent.trim().toLowerCase();
            //const bText = allOriginalRows[b].cells[index].textContent.trim().toLowerCase();
            const aTableCellText = allOriginalRows[a].cells[index].textContent.trim().toLowerCase();
            const bTableCellText = allOriginalRows[b].cells[index].textContent.trim().toLowerCase();

            //F�rs�ker omvandla texten till ett numeriskt v�rde.
            //Ers�tter , med . f�r att hantera t.ex. svenska decimaltal (t.ex. "3,14" ? "3.14").
            //parseFloat konverterar till flyttal (decimaltal).
            const aTryTextAsANumber = parseFloat(aTableCellText.replace(',', '.'));
            const bTryTextAsANumber = parseFloat(bTableCellText.replace(',', '.'));

            //Om b�da v�rdena kunde tolkas som tal.
            const aIsNumber = /^\d+([.,]\d+)?$/.test(aTableCellText);
            const bIsNumber = /^\d+([.,]\d+)?$/.test(bTableCellText);

            if (aIsNumber && bIsNumber) {

                //Sortera numeriskt.
                //Om ascending �r true, sorteras i stigande ordning(aNum - bNum).
                //Annars i fallande ordning.
                return ascending ? aTryTextAsANumber - bTryTextAsANumber : bTryTextAsANumber - aTryTextAsANumber;
            }

            //Om minst ett av v�rdena inte �r ett tal:
            //Sortera alfabetiskt med localeCompare() (tar h�nsyn till spr�kliga regler).
            //Sorteringsriktning beror p� ascending.
            return ascending ? aTableCellText.localeCompare(bTableCellText, 'sv') : bTableCellText.localeCompare(aTableCellText, 'sv');
        });

        //Sparar information om vilken kolumn som nu �r sorterad, och i vilken riktning (stigande/fallande).
        //Anv�nds f�r att veta hur n�sta klick ska hanteras.
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

    // H�ndelse f�r s�kf�ltet s� att det filtrerar medan man skriver
    searchInput.addEventListener("input", filterRowsBasedOnSearchInput);

    // Klickh�ndelser p� tabellhuvuden f�r att m�jligg�ra sortering
    const tableColumnHeaders = table.querySelectorAll("thead th");
    tableColumnHeaders.forEach((th, index) => {
        th.style.cursor = "pointer"; // �ndra muspekaren till pekande hand
        th.addEventListener("click", () => sortByColumn(index)); // Sortera efter vald kolumn
    });

    // Klickh�ndelser p� varje cell f�r att navigera till produktdetaljer
    document.querySelectorAll("#productsTable td[data-product-id]").forEach(cell => {
        cell.addEventListener("click", (event) => {

            // event.stopPropagation() � F�rhindrar att klick h�ndelsen bubblar upp och
            // p�verkar andra element(som form eller andra klickbara saker).
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