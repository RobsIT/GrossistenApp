document.addEventListener("DOMContentLoaded", function () {
    const rowsPerPage = 10;
    let currentPage = 1;

    const table = document.getElementById("productsTable");
    const tbody = document.getElementById("productsBody");

    const searchInput = document.getElementById("searchInput");
    const paginationButtons = document.getElementById("paginationButtons");

    const allOriginalRows = Array.from(tbody.getElementsByTagName("tr"));

    //Makes a copy of filterable rows
    let allFilterableRows = [...allOriginalRows];
    let currentSortedColumn = { column: null, ascending: true };

    let filteredRowIndexes = Array.from(allFilterableRows).map((_, i) => i);

    function displayOnlyFilteredRowsWithPagination() {

        //All rows become hidden from start
        allOriginalRows.forEach(row => row.style.display = "none");

        //Calculate start and end index for first and last row for current page
        const startIndexForCurrentPageRows = (currentPage - 1) * rowsPerPage;
        const endIndexForCurrentPageRows = startIndexForCurrentPageRows + rowsPerPage;

        //Only display specifikc rows for the current page
        for (let i = startIndexForCurrentPageRows; i < endIndexForCurrentPageRows && i < filteredRowIndexes.length; i++) {
            const row = allOriginalRows[filteredRowIndexes[i]];
            row.style.display = "";
        }
    }

    function renderPaginationButtons() {

        //Clear buttons from before
        paginationButtons.innerHTML = "";

        const totalPages = Math.ceil(filteredRowIndexes.length / rowsPerPage);

        //Dont show buttons if only one page
        if (totalPages <= 1) return;

        for (let i = 1; i <= totalPages; i++) {
            const btn = document.createElement("button");
            btn.textContent = i;
            btn.classList.add("pagination-btn");

            if (i === currentPage) {

                btn.classList.add("activePageButton");
            }

            btn.addEventListener("click", () => {
                currentPage = i;
                displayOnlyFilteredRowsWithPagination();
                renderPaginationButtons();
            });

            //Add child button-element in parent paginationButtons-element
            paginationButtons.appendChild(btn);
        }
    }

    //The filtered out rows only gets hidden to stil able their inputs
    //for all rows in the form, even if they are filtered out. 
    function filterRowsBasedOnSearchInput() {
        const queryFromSearchInput = searchInput.value.toLowerCase();
        const rowsInTable = document.querySelectorAll("#productsBody tr");

        //Keeps count of which rows to display
        filteredRowIndexes = [];

        rowsInTable.forEach((row, index) => {
            const matchToTextContent = Array.from(row.cells).some(cell =>
                cell.textContent.toLowerCase().includes(queryFromSearchInput)
            );

            if (matchToTextContent) {
                filteredRowIndexes.push(index);
            }

            //All rows gets hidden initially - gets displayed later by pagination
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

            //Ignore colums without sorting 
            if (!label) return;

            //Reset head-text without arrow
            th.innerHTML = label;

            //Add arrow on actual column
            if (index === currentSortedColumn.column) {
                th.innerHTML += currentSortedColumn.ascending ? " &#9660;" : " &#9650;";//Pil ner och pil upp
            }
        });
    }

    function sortByColumn(index) {

        //Checks if the clicked column is the same as the currently sorted one and toggles or sets ascending order accordingly
        const ascending = currentSortedColumn.column === index ? !currentSortedColumn.ascending : true;

        //Sorts filteredRowIndexes based on the selected column, comparing row indexes a and b
        filteredRowIndexes.sort((a, b) => {

            //Retrieves and normalizes the text from the cells in the selected column for case -insensitive comparison
            const aTableCellText = allOriginalRows[a].cells[index].textContent.trim().toLowerCase();
            const bTableCellText = allOriginalRows[b].cells[index].textContent.trim().toLowerCase();

            //Attempts to convert the text to a numeric value by replacing commas with dots and using parseFloat for decimal numbers
            const aTryTextAsANumber = parseFloat(aTableCellText.replace(',', '.'));
            const bTryTextAsANumber = parseFloat(bTableCellText.replace(',', '.'));

            //If both values could be seen as numbers
            const aIsNumber = /^\d+([.,]\d+)?$/.test(aTableCellText);
            const bIsNumber = /^\d+([.,]\d+)?$/.test(bTableCellText);

            if (aIsNumber && bIsNumber) {

                //Sort by numbers in ascending order if ascending is true, otherwise in descending order
                return ascending ? aTryTextAsANumber - bTryTextAsANumber : bTryTextAsANumber - aTryTextAsANumber;
            }

            //If at least one value is not a number. Sort alphabetically using localeCompare(), with direction based on ascending
            return ascending ? aTableCellText.localeCompare(bTableCellText, 'sv') : bTableCellText.localeCompare(aTableCellText, 'sv');
        });

        //Stores which column is currently sorted and in which direction to determine how the next click should be handled
        currentSortedColumn = { column: index, ascending };

        //Move rows accordinly to DOM sorting result
        const tbody = document.querySelector("tbody");
        filteredRowIndexes.forEach(index => {
            tbody.appendChild(allOriginalRows[index]);
        });

        displayOnlyFilteredRowsWithPagination();
        renderPaginationButtons();
        updateTableHeadSortArrows();
    }

    //To sort directly while writing search text
    searchInput.addEventListener("input", filterRowsBasedOnSearchInput);

    //Click on table heads to make sorting possible by column
    const tableColumnHeaders = table.querySelectorAll("thead th");
    tableColumnHeaders.forEach((th, index) => {
        th.style.cursor = "pointer";
        th.addEventListener("click", () => sortByColumn(index));
    });

    //To give a table-cell the click-event to get to a specific product details 
    document.querySelectorAll("#productsTable td[data-product-id]").forEach(cell => {
        cell.addEventListener("click", (event) => {

            //event.stopPropagation() – Prevents the click event from bubbling up and affecting
            // other elements (like forms or other clickable items)
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

