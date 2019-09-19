Following classes should be updated before y=using this code:

1- Services/LexSystemInfo.cs
   
    - Update GetFingerPrint() function based on some unique id. Ensure minimum length is 64.

    - Update GetOsVersion() function to get the correct Android OS version.


2- Services/LexDataStore.cs

    - Update SaveValue() function to add persistent storage
    
    - Update GetValue() function to add persistent storage