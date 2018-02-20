# BleServer
This project contains Bluetooth Low Energy server.

The actual implementation of the BLE communication in this repository uses `UWP` BLE library.
Replacing this component with Linux/OSX/... BLE library would enable the server in other operating systems.

## What am I getting using this server?
This server gives basic abstration on top of BLE client-server communication. and ability to unit test the communication channel between the twos.

It also leaves the hastle of managing BLE low level code in your application
