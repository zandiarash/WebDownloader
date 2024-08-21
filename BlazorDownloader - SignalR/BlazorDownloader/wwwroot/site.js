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
