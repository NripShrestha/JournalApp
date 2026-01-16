window.quillEditor = {
    quill: null,

    init: function (editorId) {
        this.quill = new Quill('#' + editorId, {
            theme: 'snow',
            placeholder: 'Write your thoughts...',
            modules: {
                toolbar: [
                    ['bold', 'italic', 'underline'],
                    [{ 'header': [1, 2, 3, false] }],
                    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                    ['link'],
                    ['clean']
                ]
            }
        });
    },

    getHtml: function () {
        return this.quill.root.innerHTML;
    },

    setHtml: function (content) {
        this.quill.root.innerHTML = content;
    }
};
