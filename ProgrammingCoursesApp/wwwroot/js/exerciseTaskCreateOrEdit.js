//pievienot atbildes varianta lauku (pareizība)
function addCorrectAnswerRadioInput() {
    var table = document.getElementById("possibleAnswerItems");
    //pēdējā tabulas rinda
    var row = table.rows[table.rows.length - 1];

    //lauka pievienošana - lauks "vai ir pareizi"
    let td = document.createElement("td");
    let div = document.createElement("div");
    div.setAttribute("class", "form-group");

    let isCorrectField = document.createElement("input");
    isCorrectField.setAttribute("type", "radio");
    isCorrectField.setAttribute("id", "answer" + i);
    isCorrectField.setAttribute("value", i);
    isCorrectField.setAttribute("name", "Exercise.AnswerId");
    isCorrectField.setAttribute("class", "form-control isCorrectBox");

    //kļūdas paziņojums laukam "vai ir pareizi"
    let errorForCorrectField = document.createElement("span");
    errorForCorrectField.setAttribute("class", "text-danger field-validation-valid");
    errorForCorrectField.setAttribute("data-valmsg-for", "Exercise.AnswerId");
    errorForCorrectField.setAttribute("data-valmsg-replace", "true");

    div.appendChild(isCorrectField);
    div.appendChild(errorForCorrectField);
    td.appendChild(div);
    row.appendChild(td);
}

//pievienot atbildes varianta lauku (teksts)
function addPossibleItemInput(event) {
    event.preventDefault();
    var tableBody = $("table#possibleAnswerItems tbody");

    if (i >= 2) {
        $('#deleteRowButton').removeAttr('hidden');
    }

    var nextIndex = $("table#possibleAnswerItems tbody tr").length;
    $.validator.unobtrusive.parse($('form'));
    $.ajax({
        url: addPossibleAnswerAction,
        type: 'POST',
        async: false,
        data: { index: nextIndex },
        success: function (results) {
            tableBody.append(results);
            $("form").removeData("validator");
            $("form").removeData("unobtrusiveValidation");
            $.validator.unobtrusive.parse("form");
            addCorrectAnswerRadioInput();
        }
    });

    i++;
}

//dzēst atbildes variantu
function removePossibleItemInput(event) {
    event.preventDefault();
    var table = document.getElementById("possibleAnswerItems");
    table.deleteRow(-1); //dzēst pēdējo rindu tabulā

    i--;

    if (i == 2) {
        $('#deleteRowButton').attr("hidden", true);
    }
}