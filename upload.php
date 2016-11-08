<?php
$allowedExts = array("pdf", "jpg", "png");
$temp = explode(".", $_FILES["file"]["name"]);
$extension = end($temp);
if ((($_FILES["file"]["type"] == "application/pdf")
        || ($_FILES["file"]["type"] == "image/jpeg")
        || ($_FILES["file"]["type"] == "image/jpg")
        || ($_FILES["file"]["type"] == "image/pjpeg")
        || ($_FILES["file"]["type"] == "image/x-png")
        || ($_FILES["file"]["type"] == "image/png"))
    && ($_FILES["file"]["size"] < 20480000)
    && in_array($extension, $allowedExts))
{
    if ($_FILES["file"]["error"] > 0)
    {
        echo "Error：: " . $_FILES["file"]["error"] . "<br>";
    }
    else
    {
        echo "File Name: " . chop($_FILES["file"]["name"],strrchr($_FILES["file"]["name"], '.')) . "<br>";
        echo "File Type: " . substr(strrchr($_FILES["file"]["type"], '/'), 1) . "<br>";
        echo "File Size: " . ($_FILES["file"]["size"] / 1024) . " kB<br>";
        echo "File Current Location: " . $_FILES["file"]["tmp_name"];

        if (file_exists("FileUploads/" . $_FILES["file"]["name"]))
        {
            echo $_FILES["file"]["name"] . " => The file in this name has already existed !";
        }
        else
        {
            $con = new mysqli('mysql5006.smarterasp.net', 'a117c6_phd', 'phd12345', 'db_a117c6_phd');
            $error = $_FILES["file"]["error"];
            $tmp_name = $_FILES["file"]["tmp_name"];
            $size = $_FILES["file"]["size"];
            $name = chop($_FILES["file"]["name"],strrchr($_FILES["file"]["name"], '.'));
            $type = substr(strrchr($_FILES["file"]["type"], '/'), 1);

            if ($error == UPLOAD_ERR_OK && $size > 0) {
                $fp = fopen($tmp_name, 'r');
                $content = fread($fp, $size);
                fclose($fp);
                $content = addslashes($content);
                $sql = "INSERT INTO documents (DocID,DocTitle,DocBody,TripID,DocType)"
                    . " VALUES (null,'$name', '$content',1, '$type')";
                mysqli_query( $con,$sql);

            } else {
                echo "Database Save for upload failed.\n";
            }
            mysqli_close($con);
           // move_uploaded_file($_FILES["file"]["tmp_name"], "FileUploads/" . $_FILES["file"]["name"]);
           // echo "File Now : " . "FileUploads/" . $_FILES["file"]["name"];
        }
    }
}
else
{
    echo "Invaild File Format !!!";
}
?>