describe('Invoices', () => {
  const mockInvoices = {
    items: [
      {
        id: 'inv-1',
        invoiceNumber: 'INV-20260001',
        invoiceDate: '2026-01-15T00:00:00Z',
        dueDate: '2026-02-14T00:00:00Z',
        currency: 'USD',
        status: 'Draft',
        senderCompanyName: 'My Company',
        customerCompanyName: 'Acme Corp',
        grandTotal: 6900.0,
        createdAt: '2026-01-15T00:00:00Z',
      },
      {
        id: 'inv-2',
        invoiceNumber: 'INV-20260002',
        invoiceDate: '2026-01-20T00:00:00Z',
        dueDate: '2026-02-19T00:00:00Z',
        currency: 'EUR',
        status: 'Paid',
        senderCompanyName: 'My Company',
        customerCompanyName: 'Widget Inc',
        grandTotal: 2400.0,
        createdAt: '2026-01-20T00:00:00Z',
      },
    ],
    page: 1,
    pageSize: 10,
    totalCount: 2,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
  };

  const mockCustomers = {
    items: [
      {
        id: 'cust-1',
        companyName: 'Acme Corp',
        contactPerson: 'John',
        address: '123 Main',
        email: 'john@acme.com',
        postalCode: '12345',
        vatTaxId: null,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z',
      },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 1,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
  };

  const mockSenders = {
    items: [
      {
        id: 'send-1',
        companyName: 'My Company',
        contactPerson: 'Alice',
        address: '100 Biz Blvd',
        email: 'alice@myco.com',
        phone: '+1234567890',
        vatTaxId: null,
        iban: null,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z',
      },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 1,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
  };

  const mockInvoiceDetail = {
    id: 'inv-1',
    invoiceNumber: 'INV-20260001',
    invoiceDate: '2026-01-15T00:00:00Z',
    dueDate: '2026-02-14T00:00:00Z',
    currency: 'USD',
    taxRate: 15.0,
    notes: 'Payment due within 30 days',
    status: 'Draft',
    senderId: 'send-1',
    senderCompanyName: 'My Company',
    customerId: 'cust-1',
    customerCompanyName: 'Acme Corp',
    lineItems: [
      { id: 'li-1', description: 'Consulting', quantity: 10, unitPrice: 150.0, total: 1500.0 },
      { id: 'li-2', description: 'Development', quantity: 20, unitPrice: 200.0, total: 4000.0 },
      { id: 'li-3', description: 'Management', quantity: 5, unitPrice: 100.0, total: 500.0 },
    ],
    subTotal: 6000.0,
    taxAmount: 900.0,
    grandTotal: 6900.0,
    createdAt: '2026-01-15T00:00:00Z',
    updatedAt: '2026-01-15T00:00:00Z',
  };

  beforeEach(() => {
    cy.intercept('GET', '/api/invoices?page=*', { statusCode: 200, body: mockInvoices }).as(
      'getInvoices'
    );
  });

  it('displays invoice list with status badges and totals', () => {
    cy.visit('/invoices');
    cy.wait('@getInvoices');

    cy.contains('Invoices');
    cy.contains('INV-20260001');
    cy.contains('INV-20260002');
    cy.contains('Acme Corp');
    cy.contains('Widget Inc');

    // Status badges
    cy.contains('Draft');
    cy.contains('Paid');

    // Totals
    cy.contains('$6,900.00');
    cy.contains('Showing 1 to 2 of 2 results');
  });

  it('opens invoice view modal with full details', () => {
    cy.intercept('GET', '/api/invoices/inv-1', {
      statusCode: 200,
      body: mockInvoiceDetail,
    }).as('getInvoice');

    cy.visit('/invoices');
    cy.wait('@getInvoices');

    cy.get('table tbody tr').first().find('button[title="View"]').click();
    cy.wait('@getInvoice');

    cy.contains('Invoice Details');
    cy.contains('INV-20260001');
    cy.contains('My Company');
    cy.contains('Acme Corp');

    // Line items
    cy.contains('Consulting');
    cy.contains('Development');
    cy.contains('Management');

    // Totals
    cy.contains('$6,000.00');
    cy.contains('$900.00');
    cy.contains('$6,900.00');

    // Notes
    cy.contains('Payment due within 30 days');

    // PDF download button
    cy.contains('button', 'Download PDF');
  });

  it('opens create invoice form with customer/sender dropdowns and line items', () => {
    cy.intercept('GET', '/api/customers*', { statusCode: 200, body: mockCustomers }).as(
      'getCustomers'
    );
    cy.intercept('GET', '/api/senders*', { statusCode: 200, body: mockSenders }).as(
      'getSenders'
    );

    const createdInvoice = {
      ...mockInvoiceDetail,
      id: 'inv-new',
      invoiceNumber: 'INV-20260003',
    };

    cy.intercept('POST', '/api/invoices', { statusCode: 201, body: createdInvoice }).as(
      'createInvoice'
    );

    cy.visit('/invoices');
    cy.wait('@getInvoices');

    cy.contains('button', 'New Invoice').click();
    cy.wait('@getCustomers');
    cy.wait('@getSenders');

    cy.contains('New Invoice');

    // Select customer and sender
    cy.get('select').eq(0).select('Acme Corp');
    cy.get('select').eq(1).select('My Company');

    // Fill first line item
    cy.get('[data-cy="line-items"] tbody tr').first().within(() => {
      cy.get('input').eq(0).type('Consulting Services');
      cy.get('input').eq(1).focus().type('{selectall}10');
      cy.get('input').eq(2).focus().type('{selectall}150');
    });

    // Verify live calculation: 10 * 150 = 1500
    cy.contains('1,500.00');

    // Add another line item
    cy.contains('button', 'Add Row').click();
    cy.get('[data-cy="line-items"] tbody tr').should('have.length', 2);

    cy.get('[data-cy="line-items"] tbody tr').eq(1).within(() => {
      cy.get('input').eq(0).type('Development');
      cy.get('input').eq(1).focus().type('{selectall}20');
      cy.get('input').eq(2).focus().type('{selectall}200');
    });

    // Verify updated subtotal: 1500 + 4000 = 5500
    cy.get('.space-y-2').last().within(() => {
      cy.contains('5,500.00');
    });

    // Submit
    cy.contains('button', 'Create Invoice').click();
    cy.wait('@createInvoice');

    cy.get('@createInvoice')
      .its('request.body')
      .should('deep.include', {
        customerId: 'cust-1',
        senderId: 'send-1',
      });

    cy.get('@createInvoice')
      .its('request.body.lineItems')
      .should('have.length', 2);
  });

  it('removes a line item row', () => {
    cy.intercept('GET', '/api/customers*', { statusCode: 200, body: mockCustomers }).as(
      'getCustomers'
    );
    cy.intercept('GET', '/api/senders*', { statusCode: 200, body: mockSenders }).as(
      'getSenders'
    );

    cy.visit('/invoices');
    cy.wait('@getInvoices');

    cy.contains('button', 'New Invoice').click();
    cy.wait('@getCustomers');
    cy.wait('@getSenders');

    // Add a second row
    cy.contains('button', 'Add Row').click();
    cy.get('[data-cy="line-items"] tbody tr').should('have.length', 2);

    // Remove the first row
    cy.get('[data-cy="line-items"] tbody tr').first().find('button').click();
    cy.get('[data-cy="line-items"] tbody tr').should('have.length', 1);
  });

  it('opens edit invoice form with pre-filled data', () => {
    cy.intercept('GET', '/api/invoices/inv-1', {
      statusCode: 200,
      body: mockInvoiceDetail,
    }).as('getInvoice');
    cy.intercept('GET', '/api/customers*', { statusCode: 200, body: mockCustomers }).as(
      'getCustomers'
    );
    cy.intercept('GET', '/api/senders*', { statusCode: 200, body: mockSenders }).as(
      'getSenders'
    );
    cy.intercept('PUT', '/api/invoices/inv-1', {
      statusCode: 200,
      body: { ...mockInvoiceDetail, status: 'Sent' },
    }).as('updateInvoice');

    cy.visit('/invoices');
    cy.wait('@getInvoices');

    cy.get('table tbody tr').first().find('button[title="Edit"]').click();
    cy.wait('@getInvoice');
    cy.wait('@getCustomers');
    cy.wait('@getSenders');

    cy.contains('Edit Invoice INV-20260001');

    // Verify line items are loaded
    cy.get('[data-cy="line-items"] tbody tr').should('have.length', 3);

    // Status dropdown should be visible in edit mode
    cy.get('select').eq(3).should('have.value', 'Draft');
    cy.get('select').eq(3).select('Sent');

    cy.contains('button', 'Update Invoice').click();
    cy.wait('@updateInvoice');

    cy.get('@updateInvoice')
      .its('request.body')
      .should('deep.include', { status: 'Sent' });
  });

  it('deletes an invoice with confirmation', () => {
    cy.intercept('DELETE', '/api/invoices/inv-1', { statusCode: 204 }).as('deleteInvoice');

    cy.visit('/invoices');
    cy.wait('@getInvoices');

    cy.get('table tbody tr').first().find('button[title="Delete"]').click();
    cy.contains('Delete Invoice');
    cy.contains('This action cannot be undone');
    cy.contains('button', 'Delete').click();
    cy.wait('@deleteInvoice');
  });

  it('triggers PDF download', () => {
    cy.intercept('GET', '/api/invoices/inv-1/pdf', {
      statusCode: 200,
      headers: { 'content-type': 'application/pdf' },
      body: new Blob(['%PDF-fake'], { type: 'application/pdf' }),
    }).as('downloadPdf');

    cy.visit('/invoices');
    cy.wait('@getInvoices');

    cy.get('table tbody tr').first().find('button[title="Download PDF"]').click();
    cy.wait('@downloadPdf');
  });

  it('shows empty state when no invoices', () => {
    cy.intercept('GET', '/api/invoices?page=*', {
      statusCode: 200,
      body: {
        items: [],
        page: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false,
      },
    }).as('getEmpty');

    cy.visit('/invoices');
    cy.wait('@getEmpty');

    cy.contains('No invoices yet');
    cy.contains('Create your first invoice');
  });
});
