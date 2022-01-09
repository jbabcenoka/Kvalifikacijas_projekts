tinyMCE.init({
    selector: '.editorHtml',
    mode: "textareas",
    theme: "modern",
    menubar: false,
    plugins: [
        'advlist autolink lists charmap print preview',
        'searchreplace visualblocks code fullscreen',
        'table contextmenu paste code'
    ],
    toolbar: 'undo redo | insert | styleselect | bold italic | alignleft aligncenter alignright alignjustify |  bullist numlist outdent indent'
});