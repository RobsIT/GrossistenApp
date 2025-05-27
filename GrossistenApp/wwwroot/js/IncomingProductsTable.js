// Vänta tills hela HTML-dokumentet har laddats innan koden körs
document.addEventListener("DOMContentLoaded", function () {
    const rowsPerPage = 10; // Antal rader att visa per sida
    let currentPage = 1; // Den aktuella sidan (startar på 1)

    // Hämta relevanta DOM-element
    const table = document.getElementById("productsTable");
    const tbody = document.getElementById("productsBody");
    const allOriginalRows = Array.from(tbody.getElementsByTagName("tr")); // Alla rader i tabellen

    const searchInput = document.getElementById("searchInput"); // Sökfältet
    const pagination = document.getElementById("paginationButtons"); // Behållare för sidnavigeringsknappar

    let allFilterableRows = [...allOriginalRows]; // Kopia av rader, som kan filtreras
    let currentSortedColumn = { column: null, ascending: true }; // Håller koll på vilken kolumn som är sorterad och i vilken riktning

    let filteredRowIndexes = Array.from(allOriginalRows).map((_, i) => i); // Alla rader visas initialt

    renderFilteredRowsWithPagination();
    renderPaginationButtons();
    

    // Lägg till klickhändelser på varje cell för att navigera till produktdetaljer
    document.querySelectorAll("#productsTable td[data-product-id]").forEach(cell => {
        cell.addEventListener("click", (event) => {
            // Undvik att bubbla upp till andra element
            // event.stopPropagation() – Förhindrar att klick händelsen bubblar upp och
            // påverkar andra element(som form eller andra klickbara saker).
            event.stopPropagation();

            const productId = cell.getAttribute("data-product-id");
            if (productId) {
                window.location.href = `SpecificProductDetails?id=${productId}`;
            }
        });
    });

    // Funktion för att visa rader på aktuell sida
    function renderTableRows() {
        const start = (currentPage - 1) * rowsPerPage;
        const end = start + rowsPerPage;

        const rows = document.querySelectorAll("#productsBody tr");

        rows.forEach((row, index) => {
            // Endast visa de rader som är inom den aktuella sidans intervall
            row.style.display = (index >= start && index < end) ? "" : "none";
        });
    }

    // Funktion för att skapa och visa sidnavigeringsknappar
    function renderPaginationButtons() {
        pagination.innerHTML = ""; // Töm tidigare knappar

        const totalPages = Math.ceil(filteredRowIndexes.length / rowsPerPage);
        if (totalPages <= 1) return; // Visa inte om bara en sida

        for (let i = 1; i <= totalPages; i++) {
            const btn = document.createElement("button");
            btn.textContent = i;
            btn.classList.add("pagination-btn");

            if (i === currentPage) {
                
                btn.classList.add("activePageButton");//Css klass
            }

            btn.addEventListener("click", () => {
                currentPage = i;
                renderFilteredRowsWithPagination();
                renderPaginationButtons(); // Uppdatera knapparnas stil
            });

            pagination.appendChild(btn);
        }
    }

    // Funktion för att filtrera rader baserat på söktext.
    //  Raderna döljs bara vid filtrering vilket möjliggör att alla rader kommer med i input-form även
    //  fast dom är bortfiltrerade för tillfället.
    function filterRowsBasedOnSearchInput() {
        const query = searchInput.value.toLowerCase();
        const rows = document.querySelectorAll("#productsBody tr");

        filteredRowIndexes = []; // Håller reda på vilka rader som ska visas

        rows.forEach((row, index) => {
            const match = Array.from(row.cells).some(cell =>
                cell.textContent.toLowerCase().includes(query)
            );

            if (match) {
                filteredRowIndexes.push(index);
            }

            // Dölj alla rader initialt – visas senare av paginering
            row.style.display = "none";
        });

        currentPage = 1;
        renderFilteredRowsWithPagination();
        renderPaginationButtons();
    }

    function renderFilteredRowsWithPagination() {
        const rows = document.querySelectorAll("#productsBody tr");

        const start = (currentPage - 1) * rowsPerPage;
        const end = start + rowsPerPage;

        // Dölj alla först
        rows.forEach(row => row.style.display = "none");

        // Visa bara de filtrerade, i sorterad ordning
        filteredRowIndexes.forEach((rowIndex, i) => {
            if (i >= start && i < end) {
                rows[rowIndex].style.display = "";
            }
        });
    }

    // Funktion för att sortera rader enligt angiven kolumn
    function sortByColumn(index) {
        const ascending = currentSortedColumn.column === index ? !currentSortedColumn.ascending : true;

        // Sortera filteredRowIndexes baserat på vald kolumn
        filteredRowIndexes.sort((a, b) => {
            const aText = allOriginalRows[a].cells[index].textContent.trim().toLowerCase();
            const bText = allOriginalRows[b].cells[index].textContent.trim().toLowerCase();

            const aNum = parseFloat(aText.replace(',', '.'));
            const bNum = parseFloat(bText.replace(',', '.'));

            if (!isNaN(aNum) && !isNaN(bNum)) {
                return ascending ? aNum - bNum : bNum - aNum;
            }

            return ascending ? aText.localeCompare(bText) : bText.localeCompare(aText);
        });

        currentSortedColumn = { column: index, ascending };

        updateHeaderSortArrows();
        renderFilteredRowsWithPagination();
        renderPaginationButtons();
    }

    function updateHeaderSortArrows() {
        const headers = table.querySelectorAll("thead th");

        headers.forEach((th, i) => {
            // Ta bort gamla pilar
            th.textContent = th.getAttribute("data-column");

            // Lägg till pil på aktuell kolumn
            if (i === currentSortedColumn.column) {
                th.innerHTML += currentSortedColumn.ascending ? " &#9650;" : " &#9660;";//Pil upp och pil ner
            }
        });
    }

    // Lägg till klickhändelser på tabellhuvuden för att möjliggöra sortering
    const headers = table.querySelectorAll("thead th");
    headers.forEach((th, index) => {
        th.style.cursor = "pointer"; // Ändra muspekaren till pekande hand
        th.addEventListener("click", () => sortByColumn(index)); // Sortera efter vald kolumn
    });

    // Lägg till händelse för sökfältet så att det filtrerar medan man skriver
    searchInput.addEventListener("input", filterRowsBasedOnSearchInput);

    // Initiera första renderingen av tabellen
    renderTableRows();



});

