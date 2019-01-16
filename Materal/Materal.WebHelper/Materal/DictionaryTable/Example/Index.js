class DictionaryTableIndexViewModel {
    constructor() {
        this.tableData = new Materal.Dictionary();
        for (let i = 0; i < 10000; i++) {
            this.tableData.set(`temp${i}`, { Name: `Name${i}`, Value: `Value${i}`, Temp: { AA: `AA${i}`, BB: i } });
        }
        this.dictionaryTable = new Materal.Component.DictionaryTable("dicTable", this.tableData, 10);
        const btnUp = document.getElementById("BtnUp");
        btnUp.addEventListener("click", (event) => {
            this.dictionaryTable.dataIndex--;
            this.dictionaryTable.updateTable();
        });
        const btnDown = document.getElementById("BtnDown");
        btnDown.addEventListener("click", (event) => {
            this.dictionaryTable.dataIndex++;
            this.dictionaryTable.updateTable();
        });
        const dicTable = document.getElementById("dicTable");
        dicTable.addEventListener("mousewheel", (event) => {
            if (event.deltaY > 0) {
                this.dictionaryTable.dataIndex++;
            }
            else {
                this.dictionaryTable.dataIndex--;
            }
            this.dictionaryTable.updateTable();
        });
    }
}
window.addEventListener("load", () => {
    const viewModel = new DictionaryTableIndexViewModel();
});
//# sourceMappingURL=Index.js.map