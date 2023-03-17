import { Map } from "@pbe/react-yandex-maps";

const Maps = () => {
    return (
        <Map defaultState={{ center: [55.751574, 37.573856], zoom: 9 }} className="w-100 h-100" />
    );
}

export default Maps;