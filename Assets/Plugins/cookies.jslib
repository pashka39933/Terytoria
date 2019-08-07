mergeInto(LibraryManager.library, {

  	SetUUID: function (uuid) {
		uuid = Pointer_stringify(uuid);
		var date = new Date();
	        date.setTime(date.getTime() + (365*24*60*60*1000));
	        expires = "; expires=" + date.toUTCString();
		document.cookie = "uuid=" + uuid + expires + "; path=/";
  	},

  	GetUUID: function () {
    		var nameEQ = "uuid=";
		var ca = document.cookie.split(';');
		var returnStr = "";
		for(var i=0; i < ca.length; i++) 
		{
		        var c = ca[i];
        		while (c.charAt(0)==' ') c = c.substring(1,c.length);
        		if (c.indexOf(nameEQ) == 0) 
			{
				returnStr = c.substring(nameEQ.length,c.length);
				break;
			}
		}
		var bufferSize = lengthBytesUTF8(returnStr) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(returnStr, buffer, bufferSize);
		return buffer;
  	},

	DeleteUUID: function () {
		document.cookie = "uuid=; Max-Age=-99999999;";
        }

});
