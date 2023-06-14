//  Makes sure any file uploaded is not too large...
const fileInputClass = "illustration-file";
const maxAllowedSize = 15 * 1024 * 1024;
const allowedTypes = ['image/jpeg', 'image/png'];
const pageFileInputs = document.getElementsByClassName(fileInputClass);

for (let i = 0; i < pageFileInputs.length; i++) {
    const input = pageFileInputs[i] as HTMLInputElement;

    input.addEventListener("change", function (): void {
        if (input.files.length == 0) return;

        if (input.files[0].size > maxAllowedSize) {
            input.value = '';
        }

        if (!allowedTypes.includes(input.files[0].type)) {
            input.value = '';
        }
    });
}
