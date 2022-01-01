function deleteUser(userId, userEmail) {
    if (confirm('Are you sure you want to delete selected user? '
        + 'User`s results and all user`s courses will be deleted if user is courses creator or administrator.')) {
        $.ajax({
            type: "POST",
            url: deleteAction,
            data: JSON.stringify(userId),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (result) {
                if (result.isDeleted) {
                    alert("User successfully deleted");
                    removeUserFromTable(userEmail);
                }
                else {
                    alert("Failed to delete user");
                }
            }
        });
    }
    else {
        return false;
    }
}

function removeUserFromTable(userEmail) {
    var rows = document.getElementById("users-table").getElementsByTagName("tr");
    var cell;

    for (var i = 0; i < rows.length; i++) {
        cell = rows[i].getElementsByTagName("td")[1];

        if (cell) {
            value = cell.innerText || cell.textContent;

            if (value.normalize() === userEmail.normalize()) {
                document.getElementById("users-table").deleteRow(i);
                return;
            }
        }
    }
}

function searchUser(inputId) {
    var filterInput = document.getElementById(inputId).value.toUpperCase();
    var rows = document.getElementById("users-table").getElementsByTagName("tr");
    var cell, column;

    if (inputId == 'userNameInput') {
        column = 0;
    }
    else if (inputId == 'userEmailInput') {
        column = 1;
    }

    for (var i = 0; i < rows.length; i++) {
        cell = rows[i].getElementsByTagName("td")[column];

        if (cell) {
            value = cell.innerText || cell.textContent;

            if (value.toUpperCase().indexOf(filterInput) > -1) {
                rows[i].style.display = "";
            }
            else {
                rows[i].style.display = "none";
            }
        }
    }
}