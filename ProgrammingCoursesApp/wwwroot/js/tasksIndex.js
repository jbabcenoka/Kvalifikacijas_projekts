$(document).ready(function () {
    $(".nav-tabs a").on('show.bs.tab', function (e) {
        var currentTab = $(this);

        if (!currentTab.hasClass('exercise')) //video, lasamais materials
        {
            //atzīmēt, ka ir izlasīts
            currentTab.addClass('readed');
            $(currentTab.attr("data-bs-target") + "-input").val(true);
        }
        else if (currentTab.hasClass('readed') //gadījums, kad netiek izvēlēta atbilde
            && $(currentTab.attr("data-bs-target") + ' button.answer').text() == 'Try again')
        {
            $(currentTab.attr("data-bs-target") + ' input[type=radio]:not(:checked)').prop('disabled', true);
        }
    });

    $(".nav-tabs a:first-child").tab('show');
});

function showNextTab(currentTabId) {
    $("#tab" + currentTabId).next().tab('show');
}

//iegūt rezultātu
function getResult(correctId, event) {
    event.preventDefault();
    var currentTab = $('.nav-tabs a.active');
    var target = currentTab.attr("data-bs-target");
    var checkedAnswerId = $(target + ' input:checked').attr('id');

    //ja lietotājs neizvēlējas atbildi
    if (!checkedAnswerId)
    {
        alert('Please answer the question.');
        return;
    }

    if (!currentTab.hasClass('readed'))
    {
        currentTab.addClass('readed');
    }

    var isCorrect = checkedAnswerId == correctId ? true : false;

    if (isCorrect && currentTab.hasClass('canImproveResult')) //ja bija jau rezultāts
    { 
        currentTab.removeClass('canImproveResult');
    }
    else
    {
        currentTab.addClass('canImproveResult');
    }

    if ($(target + ' button.answer').text() == 'Try again') //tiek nospiesta poga "Try again"
    { 
        $(target + ' div.resultBox').attr('hidden', true); //paslēpt rezultātu
        $(target + ' input:checked').prop("checked", false); //dzēst lietotāja izvēli
        $(target + ' input[type=radio]').prop('disabled', false); //visi lauki ir atkal pieejami atbildei
        $(target + ' button.answer').html('Answer the question'); //mainām pogas tekstu
    }
    else //tiek nospiesta poga "Answer the question"
    {
        //atzīmēt, ka ir izlasīts
        currentTab.addClass('readed');
        $(target + "-input").val(true);
        
        $(target + ' div.resultBox').removeAttr('hidden');
        $(target + ' input[type=radio]:not(:checked)').prop('disabled', true);

        if (isCorrect)
        {
            if ($(target + ' div.resultBox').hasClass('incorrectResult')) {
                $(target + ' div.resultBox').removeClass('incorrectResult');
            }
            $(target + ' div.resultBox').html('<b>Answer</b>: Your answer is correct!');
            $(target + ' div.resultBox').addClass('correctResult');
        }
        else
        {
            $(target + ' div.resultBox').html('<b>Answer</b>: Your answer is not correct.');
            $(target + ' div.resultBox').addClass('incorrectResult');
        }

        //mainām pogas tekstu
        $(target + ' button.answer').html('Try again');
    }
}