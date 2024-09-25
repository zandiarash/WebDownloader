//window.downloadFileFromStream = async (fileName, contentStreamReference) => {
//    const arrayBuffer = await contentStreamReference.arrayBuffer();
//    const blob = new Blob([arrayBuffer]);
//    const url = URL.createObjectURL(blob);
//    const anchorElement = document.createElement('a');
//    anchorElement.href = url;
//    anchorElement.download = fileName ?? '';
//    anchorElement.click();
//    anchorElement.remove();
//    URL.revokeObjectURL(url);
//};

window.clipboardCopy = {
    copyText: function (text) {
        navigator.clipboard.writeText(text).then(function () {
            console.log("Copied to clipboard!");
        })
            .catch(function (error) {
                alert(error);
            });
    }
};

function scrollToDownloads(id) {
    document.getElementById(id).scrollIntoView({ behavior: 'smooth' });
}

function FileUploaderInit(className) {
    $(`.${className}`).filepond();

    // Set FilePond options (optional)
    FilePond.setOptions({
        server: {
            url: '/uploader',
            process: {
                url: '',
                method: 'POST',
                withCredentials: false,
                headers: {},
                timeout: 7000,
                onload: null,
                onerror: null,
                ondata: null
            }
            // revert: '/revert',
            // restore: '/restore',
            // load: '/load',
            // fetch: '/fetch',
        }
    });

    const pond = FilePond.create(document.querySelector(`.${className}`), {});

    // Event listener for file upload
    pond.on('processfile', (error, file) => {
        if (error) {
            console.error('File processing failed', error);
            return;
        }
        console.log('File processed successfully', file);

        //fetch('/uploader/inform', {
        //    method: 'POST',
        //    headers: { 'Content-Type': 'application/json', },
        //    body: JSON.stringify({ content: file.file.name }),
        //})
        //.then(response => response.json())
        //.then(data => console.log('Success:', data))
        //.catch((error) => console.error('Error:', error));

        DotNet.invokeMethodAsync('BlazorDownloader', 'AddToDowloadsList', file.file.name)
            .then(data => {
                console.log(data);
            });

    });
}