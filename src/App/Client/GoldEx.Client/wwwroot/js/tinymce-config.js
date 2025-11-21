const file_picker_callback = (cb, value, meta) => {
    const input = document.createElement('input');
    input.setAttribute('type', 'file');

    if (meta.filetype === 'image') input.setAttribute('accept', 'image/*');
    else if (meta.filetype === 'media') input.setAttribute('accept', 'video/*,audio/*');

    input.addEventListener('change', async (e) => {
        const file = e.target.files[0];
        const formData = new FormData();
        formData.append('file', file);

        const response = await fetch(document.getElementById('upload-route').value, {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            alert("Upload failed");
            return;
        }

        const data = await response.json();
        cb(data.location, { title: file.name });
    });

    input.click();
}

window.createBlogEditorConfig = {
    plugins: "image media link lists table directionality advcode fullscreen",
    toolbar: [
        "newdocument print | addtemplate inserttemplate | code | cut copy paste pastetext undo redo selectall | bold italic underline strikethrough numlist bullist ltr rtl | styles fontfamily fontsizeinput forecolor backcolor subscript superscript | alignleft aligncenter alignright alignjustify alignnone indent outdent lineheight | remove removeformat | hr image media link table"
    ],
    file_picker_types: "image media",
    file_picker_callback: file_picker_callback,
    automatic_uploads: true,
    images_upload_url: document.getElementById('upload-route').value,
    promotion: false,
    cleanup_on_startup: false,
    trim_span_elements: false,
    verify_html: false,
    cleanup: false,
    convert_urls: false
};