# iCarus
predict-sync-compensate experiment

TODO
====
2017/4/23
got no idea when can i finish this, can only work on this one day per week
1. finish client-side game state machine
2. keep using ConnectionApproval feature, but do not deny player connection now, add a auth stage to verify player.
3. refactor code mostly
TODO:
- [ ] player manager sync, which only requre event sync, make server.syncmanager to support it 
- [ ] finish avatar creation and connect player to avatar
- [ ] basic avatar movement sync
- [ ] finish and test reconnect feature
- [ ] looks like message built using flatbuffer is very large in bytes though the information it contains is not too much, maybe i use it wrong, check this problem
- [ ] improve SharpFlatBuffer lib, it just sucks now

2017/4/16
- [x] finish game state machine
- [x] UdpListener expose onIncommingConnection/onConnectionStatusChanged for it can be modify from outside
- [x] implement server side player state machine
- [ ] don't use ConnectionApproval feature, implement a simple verification step in VerifyIdentity phase
- [x] test make sure both state machine works
