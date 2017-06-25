function previewImage(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        $("#imagePreview").css("display", "block");
        reader.onload = function (e) {
            $("#imagePreview").attr('src', e.target.result);
        };

        reader.readAsDataURL(input.files[0]);
    }
}