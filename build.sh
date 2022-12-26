#!/bin/bash
#sudo systemctl stop masjidbandung.service
dotnet publish MasjidBandung.csproj -c Release -r linux-arm --self-contained true
#cp *.so /home/pi/MejaPasirDrawer/bin/Release/net6.0/linux-arm/publish
#cp *.o /home/pi/MejaPasirDrawer/bin/Release/net6.0/linux-arm/publish
echo "Run (r) or deploy (d)?"
read mode

case $mode in
        r)
                echo "Run"
                sudo ./bin/Release/net6.0/linux-arm/publish/MasjidBandung
                ;;
        d)
                echo "Deploy"
                test -f /lib/systemd/system/masjidbandung.service || sudo cp masjidbandung.service /lib/systemd/system/masjidbandung.service && sudo systemctl daemon-reload && sudo systemctl enable masjidbandung
                sudo systemctl restart masjidbandung
                systemctl status masjidbandung
                ;;
        *)
                echo "Just build"
                ;;
esac

#sudo /home/pi/MejaPasirDrawer/bin/Release/net5.0/linux-arm/publish/MejaPasirDrawer
