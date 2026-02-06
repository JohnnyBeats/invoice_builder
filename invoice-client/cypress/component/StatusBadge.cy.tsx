import { StatusBadge } from '../../src/components/StatusBadge';

describe('StatusBadge', () => {
  const statuses = ['Draft', 'Sent', 'Paid', 'Overdue', 'Cancelled'];

  statuses.forEach((status) => {
    it(`renders "${status}" badge with correct text`, () => {
      cy.mount(<StatusBadge status={status} />);
      cy.get('span').should('have.text', status);
    });
  });

  it('renders Draft with gray styling', () => {
    cy.mount(<StatusBadge status="Draft" />);
    cy.get('span').should('have.class', 'bg-gray-100').and('have.class', 'text-gray-700');
  });

  it('renders Paid with green styling', () => {
    cy.mount(<StatusBadge status="Paid" />);
    cy.get('span').should('have.class', 'bg-emerald-100').and('have.class', 'text-emerald-700');
  });

  it('renders Overdue with red styling', () => {
    cy.mount(<StatusBadge status="Overdue" />);
    cy.get('span').should('have.class', 'bg-red-100').and('have.class', 'text-red-700');
  });

  it('renders unknown status with default gray styling', () => {
    cy.mount(<StatusBadge status="Unknown" />);
    cy.get('span')
      .should('have.text', 'Unknown')
      .and('have.class', 'bg-gray-100');
  });
});
