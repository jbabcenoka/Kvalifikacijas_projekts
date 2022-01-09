function GetTopicsInOrder() {
    var list = new Array();
    var elements = document.getElementsByName('item.Id');

    Array.prototype.forEach.call(elements, (function (item) {
        list.push(item.value);
    }));

    return list;
}

$("#sortable").sortable({
    stop: function (event, ui) {
        $.ajax({
            type: "GET",
            url: changeTopicsOrderAction,
            contentType: "application/json; charset=utf-8",
            traditional: true,
            data: {
                "id": courseId, "topicsInOrder": GetTopicsInOrder()
            },
            dataType: "json",
            failure: function (error) {
                alert(error);
            }
        })
    }
});