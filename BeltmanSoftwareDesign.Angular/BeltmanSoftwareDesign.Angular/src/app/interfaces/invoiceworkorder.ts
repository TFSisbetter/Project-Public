export interface InvoiceWorkorder {
    id: number;
    invoiceId: number | null;
    workorderId: number | null;
    rateId: number | null;
}