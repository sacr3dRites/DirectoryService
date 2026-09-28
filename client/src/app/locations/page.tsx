import { Metadata } from "next";
import Locations from "./locations";

export const metadata: Metadata = {
  title: "Локации | Directory Service",
};

export default function LocationsPage() {
  return <Locations />;
}
