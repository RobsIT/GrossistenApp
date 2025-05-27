// V�nta tills hela HTML-dokumentet har laddats innan koden k�rs
document.addEventListener("DOMContentLoaded", function () {
    const rowsPerPage = 10; // Antal rader att visa per sida
    let currentPage = 1; // Den aktuella sidan (startar p� 1)

    // H�mta relevanta DOM-element
    const table = document.getElementById("productsTable");
    const tbody = document.getElementById("productsBody");
    const allOriginalRows = Array.from(tbody.getElementsByTagName("tr")); // Alla rader i tabellen

    const searchInput = document.getElementById("searchInput"); // S�kf�ltet
    const pagination = document.getElementById("paginationButtons"); // Beh�llare f�r sidnavigeringsknappar

    let allFilterableRows = [...allOriginalRows]; // Kopia av rader, som kan filtreras
    let currentSortedColumn = { column: null, ascending: true }; // H�ller koll p� vilken kolumn som �r sorterad och i vilken riktning

    let filteredRowIndexes = Array.from(allOriginalRows).map((_, i) => i); // Alla rader visas initialt

    renderFilteredRowsWithPagination();
    renderPaginationButtons();
    

    // L�gg till klickh�ndelser p� varje cell f�r att navigera till produktdetaljer
    document.querySelectorAll("#productsTable td[data-product-id]").forEach(cell => {
        cell.addEventListener("click", (event) => {
            // Undvik att bubbla upp till andra element
            // event.stopPropagation() � F�rhindrar att klick h�ndelsen bubblar upp och
            // p�verkar andra element(som form eller andra klickbara saker).
            event.stopPropagation();

            const productId = cell.getAttribute("data-product-id");
            if (productId) {
                window.location.href = `SpecificProductDetails?id=${productId}`;
            }
        });
    });

    // Funktion f�r att visa rader p� aktuell sida
    function renderTableRows() {
        const start = (currentPage - 1) * rowsPerPage;
        const end = start + rowsPerPage;

        const rows = document.querySelectorAll("#productsBody tr");

        rows.forEach((row, index) => {
            // Endast visa de rader som �r inom den aktuella sidans intervall
            row.style.display = (index >= start && index < end) ? "" : "none";
        });
    }

    // Funktion f�r att skapa och visa sidnavigeringsknappar
    function renderPaginationButtons() {
        pagination.innerHTML = ""; // T�m tidigare knappar

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

    // Funktion f�r att filtrera rader baserat p� s�ktext.
    //  Raderna d�ljs bara vid filtrering vilket m�jligg�r att alla rader kommer med i input-form �ven
    //  fast dom �r bortfiltrerade f�r tillf�llet.
    function filterRowsBasedOnSearchInput() {
        const query = searchInput.value.toLowerCase();
        const rows = document.querySelectorAll("#productsBody tr");

        filteredRowIndexes = []; // H�ller reda p� vilka rader som ska visas

        rows.forEach((row, index) => {
            const match = Array.from(row.cells).some(cell =>
                cell.textContent.toLowerCase().includes(query)
            );

            if (match) {
                filteredRowIndexes.push(index);
            }

            // D�lj alla rader initialt � visas senare av paginering
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

        // D�lj alla f�rst
        rows.forEach(row => row.style.display = "none");

        // Visa bara de filtrerade, i sorterad ordning
        filteredRowIndexes.forEach((rowIndex, i) => {
            if (i >= start && i < end) {
                rows[rowIndex].style.display = "";
            }
        });
    }

    // Funktion f�r att sortera rader enligt angiven kolumn
    function sortByColumn(index) {
        const ascending = currentSortedColumn.column === index ? !currentSortedColumn.ascending : true;

        // Sortera filteredRowIndexes baserat p� vald kolumn
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

            // L�gg till pil p� aktuell kolumn
            if (i === currentSortedColumn.column) {
                th.innerHTML += currentSortedColumn.ascending ? " &#9650;" : " &#9660;";//Pil upp och pil ner
            }
        });
    }

    // L�gg till klickh�ndelser p� tabellhuvuden f�r att m�jligg�ra sortering
    const headers = table.querySelectorAll("thead th");
    headers.forEach((th, index) => {
        th.style.cursor = "pointer"; // �ndra muspekaren till pekande hand
        th.addEventListener("click", () => sortByColumn(index)); // Sortera efter vald kolumn
    });

    // L�gg till h�ndelse f�r s�kf�ltet s� att det filtrerar medan man skriver
    searchInput.addEventListener("input", filterRowsBasedOnSearchInput);

    // Initiera f�rsta renderingen av tabellen
    renderTableRows();



});

